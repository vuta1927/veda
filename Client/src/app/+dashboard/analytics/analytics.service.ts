import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { ConfigurationService } from "../../shared/services/configuration.service";
import { Observable } from "rxjs/Observable";
import { IAppCoreResponse } from "../../shared/models/appcore-response.model";
import { IDashboard, project, projectAnalist } from "../../shared/models/dashboard.model";
@Injectable()
export class DashBoardService{
    constructor(private http: HttpClient, private configurationService: ConfigurationService){}

    public getDataProject(projectId: string):Observable<IAppCoreResponse<projectAnalist>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/Dashboard/GetDataProject/' + projectId;
        let result = this.http.get<IAppCoreResponse<projectAnalist>>(url)
        return result;
    }

    public getProjectsByUser():Observable<IAppCoreResponse<project[]>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/projects/GetProjectsByUser';
        return this.http.get<IAppCoreResponse<project[]>>(url);
    }
}