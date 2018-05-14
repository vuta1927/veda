import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Class } from '../shared/models/class.model';
import { HubConnection } from '@aspnet/signalr';
import { ProjectUser, IProjectUserForMerge } from '../shared/models/project-user.model';
@Injectable()
export class DataService{
    private classSource = new BehaviorSubject<Class[]>( new Array<Class>());
    currentClass = this.classSource.asObservable();
    
    private classForMergeSource = new BehaviorSubject<Class[]>( new Array<Class>());
    currentClassForMerge = this.classForMergeSource.asObservable();

    private projectNameSource = new BehaviorSubject<string>('New Project');
    currentProjectName = this.projectNameSource.asObservable();

    private ProjectUserSource = new BehaviorSubject<IProjectUserForMerge>( new ProjectUser());
    currentProjectUser = this.ProjectUserSource.asObservable();

    constructor(){

    }
    
    changeClass(klass: Class[]){
        this.classSource.next(klass);
    }

    changeClassForMerge(klass: Class[]){
        this.classForMergeSource.next(klass);
    }

    changeProjectName(name: string){
        this.projectNameSource.next(name);
    }

    changeProjectUser(projectUser: ProjectUser){
        this.ProjectUserSource.next(projectUser);
    }
}