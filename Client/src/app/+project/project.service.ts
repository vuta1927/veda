import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { ConfigurationService } from "../shared/services/configuration.service";
import { Observable } from "rxjs/Observable";
import { IAppCoreResponse } from "../shared/models/appcore-response.model";
import { ProjectForView, ProjectForAdd, ProjectForUpdate } from "../shared/models/project.model";
@Injectable()
export class ProjectService {
    constructor(private http: HttpClient, private configurationService: ConfigurationService) {}

    public getProject(name?: string){
        let url = this.configurationService.serverSettings.apiUrl;
        if(name){
            url += '/api/projects/getprojectbyname/' + name;
        }else{
            url += '/api/projects/getprojects';
        }
        let result = this.http.get<IAppCoreResponse<ProjectForView[]>>(url)
        return result;
    }

    public getProjectById(id?:string){
        let url = this.configurationService.serverSettings.apiUrl + '/api/projects/getproject/'+id;

        return this.http.get<IAppCoreResponse<ProjectForView[]>>(url);
    }

    public UpdateProject(projectForUpdate):Observable<IAppCoreResponse<ProjectForUpdate[]>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/projects/update';

        return this.http.put<IAppCoreResponse<ProjectForUpdate[]>>(url,projectForUpdate);
    }

    public AddProject(projectForCreate):Observable<IAppCoreResponse<ProjectForAdd[]>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/projects/create';
        return this.http.post<IAppCoreResponse<ProjectForAdd[]>>(url,projectForCreate);
    }

    public DeleteProject(id):Observable<IAppCoreResponse<any>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/projects/DeleteProject/' + id;
        return this.http.delete<IAppCoreResponse<any>>(url);
    }
}