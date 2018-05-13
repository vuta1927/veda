import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Class } from '../shared/models/class.model';
import { HubConnection } from '@aspnet/signalr';
@Injectable()
export class DataService{
    private classSource = new BehaviorSubject<Class[]>( new Array<Class>());
    currentClass = this.classSource.asObservable();
    constructor(){

    }
    
    changeProject(klass: Class[]){
        this.classSource.next(klass);
    }
}