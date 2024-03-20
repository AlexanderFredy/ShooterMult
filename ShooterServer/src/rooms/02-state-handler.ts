import { Room, Client } from "colyseus";
import { Schema, type, MapSchema } from "@colyseus/schema";

export class Player extends Schema {
    @type("uint8")
    skin = 0;
    
    @type("uint8")
    loss = 0;

    @type("int8")
    maxHp = 0;

    @type("int8")
    curHp = 0;
   
    @type("number")
    speed = 0;
    
    @type("number")
    pX = 0;

    @type("number")
    pY = 0;

    @type("number")
    pZ = 0;

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

/* class SpawnPoint{
    @type("number")
    pX = 0;

    @type("number")
    pY = 0;

    constructor(x: number,y: number){
        this.pX = x;
        this.pY = y;
    }
}  */

export class State extends Schema {
    @type({ map: Player })
    players = new MapSchema<Player>();

    something = "This attribute won't be sent to the client-side";

    createPlayer(sessionId: string, data:any, skin:number) {
        const player = new Player();
        player.skin = skin;
        player.speed = data.speed;
        player.maxHp = data.hp;
        player.curHp = data.hp;
        player.pX = data.pX;
        player.pY = data.pY;
        player.pZ = data.pZ;
        player.rY = data.rY;

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
    spawnPointsCount = 1;
    skins: number[] = [0];

   /*  spawnPoints = new Map<SpawnPoint,string>();

    constructor(){
        super();
        this.spawnPoints.set(new SpawnPoint(-25,-25),"");
        this.spawnPoints.set(new SpawnPoint(25,25),"");
    } */

    mixArray(arr:any){
        var currentIndex = arr.length;
        var tempValue, randomIndex;

        while (currentIndex !== 0){
            randomIndex = Math.floor(Math.random()*currentIndex);
            currentIndex -= 1;
            tempValue = arr[currentIndex];
            arr[currentIndex] = arr[randomIndex];
            arr[randomIndex] = tempValue;
        }
    }

    onCreate (options) {
        for (var i = 1; i < options.skins; i++){
            this.skins.push(i);
        }
        this.mixArray(this.skins);

        this.spawnPointsCount = options.points;
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

                const point = Math.floor(Math.random() * this.spawnPointsCount);

                /* const sp = this.GetMySpawnPoint(clientID);
                const x = sp.pX;
                const z = sp.pY; */
                this.clients[i].send("Restart",point);
            }
        });
    }

    onAuth(client, options, req) {
        return true;
    }

    onJoin (client: Client, data:any) {
        if (this.clients.length > 1) this.lock();

        //const sp = this.GetFreeSpawnPoint(client.sessionId);

        const skin = this.skins[this.clients.length-1];
        this.state.createPlayer(client.sessionId, data, skin);
    }

    onLeave (client) {
        this.state.removePlayer(client.sessionId);
    }

    onDispose () {
        console.log("Dispose StateHandlerRoom");
    }

    /* GetFreeSpawnPoint(id:any): SpawnPoint{
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
    } */

}
