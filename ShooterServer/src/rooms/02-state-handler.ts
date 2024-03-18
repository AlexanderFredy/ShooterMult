import { Room, Client } from "colyseus";
import { Schema, type, MapSchema } from "@colyseus/schema";

export class Player extends Schema {
    @type("uint8")
    loss = 0;

    @type("int8")
    maxHp = 0;

    @type("int8")
    curHp = 0;
   
    @type("number")
    speed = 0;
    
    @type("number")
    pX = Math.floor(Math.random() * 50) - 25;

    @type("number")
    pY = 0;

    @type("number")
    pZ = Math.floor(Math.random() * 50) - 25;

    @type("number")
    vX = 0;

    @type("number")
    vY = 0;

    @type("number")
    vZ = 0;

    @type("number")
    rX = 0;

    @type("number")
    rY = 0;

    @type("number")
    avY = 0;
}

class SpawnPoint{
    @type("number")
    pX = 0;

    @type("number")
    pY = 0;

    constructor(x: number,y: number){
        this.pX = x;
        this.pY = y;
    }
} 

export class State extends Schema {
    @type({ map: Player })
    players = new MapSchema<Player>();

    something = "This attribute won't be sent to the client-side";

    createPlayer(sessionId: string, data:any, sp:SpawnPoint) {
        const player = new Player();
        player.speed = data.speed;
        player.maxHp = data.hp;
        player.curHp = data.hp;
        player.pX = sp.pX;
        player.pZ = sp.pY;

        this.players.set(sessionId, player);
    }

    removePlayer(sessionId: string) {
        this.players.delete(sessionId);
    }

    movePlayer (sessionId: string, data: any) {
        const player = this.players.get(sessionId);
        player.pX = data.pX;
        player.pY = data.pY;
        player.pZ = data.pZ;
        player.vX = data.vX;
        player.vY = data.vY;
        player.vZ = data.vZ;
        player.rX = data.rX;
        player.rY = data.rY;
        player.avY = data.avY;
    }
}

export class StateHandlerRoom extends Room<State> {
    maxClients = 2;

    spawnPoints = new Map<SpawnPoint,string>();

    constructor(){
        super();
        this.spawnPoints.set(new SpawnPoint(-25,-25),"");
        this.spawnPoints.set(new SpawnPoint(25,25),"");
    }

    onCreate (options) {
        console.log("StateHandlerRoom created!", options);

        this.setState(new State());

        this.onMessage("move", (client, data) => {
            //console.log("StateHandlerRoom received message from", client.sessionId, ":", data);
            this.state.movePlayer(client.sessionId, data);
        });

        this.onMessage("shoot", (client, data) => {
            this.broadcast("Shoot",data,{except: client});
        });

        this.onMessage("change_weapon", (client, data) => {
            this.broadcast("Change_Weapon",data,{except: client});
        });

        this.onMessage("damage", (client, data) => {
            const clientID = data.id;
            const player = this.state.players.get(clientID);

            let hp = player.curHp - data.value;

            if (hp > 0){
                player.curHp = hp;
                return;
            }

            player.loss++;
            player.curHp = player.maxHp;

            for(var i=0; i< this.clients.length; i++){
                if(this.clients[i].id != clientID) continue;

                //const x = Math.floor(Math.random() * 50) - 25;
                //const z = Math.floor(Math.random() * 50) - 25;

                const sp = this.GetMySpawnPoint(clientID);
                const x = sp.pX;
                const z = sp.pY;

                const message = JSON.stringify({x,z});
                this.clients[i].send("Restart",message);
            }
        });
    }

    onAuth(client, options, req) {
        return true;
    }

    onJoin (client: Client, data:any) {
        if (this.clients.length > 1) this.lock();
        
        const sp = this.GetFreeSpawnPoint(client.sessionId);

        client.send("hello", "world");
        this.state.createPlayer(client.sessionId, data,sp);
    }

    onLeave (client) {
        this.state.removePlayer(client.sessionId);
    }

    onDispose () {
        console.log("Dispose StateHandlerRoom");
    }

    GetFreeSpawnPoint(id:any): SpawnPoint{
        let sp = new SpawnPoint(0,0);
        for (let [key, value] of this.spawnPoints.entries())
         {
            if (value == "")
            {            
                this.spawnPoints.set(key,id);
                sp = key;
                break;    
            }          
        }
        return sp;
    }

    GetMySpawnPoint(id:any): SpawnPoint{
        let sp = new SpawnPoint(0,0);
        for (let [key, value] of this.spawnPoints.entries())
        {
            if (value == id)
            {            
                sp = key;
                break;    
            }          
        }
        return sp;
    }

}
