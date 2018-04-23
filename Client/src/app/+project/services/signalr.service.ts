import { Injectable } from '@angular/core'
import { HubConnection } from '@aspnet/signalr'
import { BehaviorSubject } from 'rxjs/BehaviorSubject'
import { ConfigurationService } from '../../shared/services/configuration.service'
@Injectable()

export class SignalRService {

    private hubSource = new BehaviorSubject<HubConnection>(new HubConnection('http://localhost:52719/project'));
    hubConnection = this.hubSource.asObservable();
    constructor() { }

    changeHub(hub: HubConnection) {
        this.hubSource.next(hub);
    }

    startHub() {
        this.hubConnection.subscribe(hub=>{
            hub.start().then(() => console.log('connection started!')).catch(err => console.log('Error while establishing connection !'));
        });
    }

    listen(method:string):any{
        let result;
        this.hubConnection.subscribe(hub=>{
            hub.on(method, data =>{
                result = data;
                console.log(data);
            });
        });
        return result;
    }

    invoke(method:string, args:any[]){
        this.hubConnection.subscribe(hub=>{
            hub.invoke(method, args).catch(err=>console.log(err));
        });
    }

    stopHub(){
        this.hubConnection.subscribe(hub=>{hub.stop();});
    }
}