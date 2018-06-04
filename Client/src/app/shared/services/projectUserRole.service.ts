import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { ConfigurationService } from "../services/configuration.service";
import { Observable } from "rxjs/Observable";
import { IAppCoreResponse } from "../models/appcore-response.model";
import {IProjectUser} from '../models/project-user.model';
@Injectable()
export class ProjectUserSecurityService{
    constructor(private http: HttpClient, private configurationService: ConfigurationService){}

    public getRoles(projectId:string):Observable<IAppCoreResponse<IProjectUser>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/ProjectUsers/getRole/' + projectId;
        return this.http.get<IAppCoreResponse<IProjectUser>>(url);
    }
}