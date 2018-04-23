import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { ProjectForView } from '../../shared/models/project.model';
import { HubConnection } from '@aspnet/signalr';
@Injectable()
export class DataService{
    private projectSource = new BehaviorSubject<ProjectForView>( new ProjectForView());
    currentProject = this.projectSource.asObservable();
    constructor(){

    }
    
    changeProject(project: ProjectForView){
        this.projectSource.next(project);
    }
}