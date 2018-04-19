import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { ConfigurationService } from "../shared/services/configuration.service";
import { Observable } from "rxjs/Observable";
import { IAppCoreResponse } from "../shared/models/appcore-response.model";
import { ProjectForView, ProjectForAdd, ProjectForUpdate } from "../shared/models/project.model";
@Injectable()
export class ProjectService {
    constructor(private http: HttpClient, private configurationService: ConfigurationService) {}

    public getProject(params:string):Observable<IAppCoreResponse<ProjectForView[]>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/projects/GetProjects/'+params;
        return this.http.get<IAppCoreResponse<ProjectForView[]>>(url);
    }

    public getTotal():Observable<IAppCoreResponse<number>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/projects/GetTotal';
        return this.http.get<IAppCoreResponse<number>>(url);
    }

    public getProjectByName(name:string):Observable<IAppCoreResponse<ProjectForView>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/projects/GetProjectByName/' + name;
        return this.http.get<IAppCoreResponse<ProjectForView>>(url);
    }

    public getProjectById(id?:string){
        let url = this.configurationService.serverSettings.apiUrl + '/api/projects/GetProject/'+id;

        return this.http.get<IAppCoreResponse<ProjectForView[]>>(url);
    }

    public UpdateProject(projectForUpdate):Observable<IAppCoreResponse<ProjectForUpdate[]>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/projects/Update';

        return this.http.put<IAppCoreResponse<ProjectForUpdate[]>>(url,projectForUpdate);
    }

    public AddProject(projectForCreate):Observable<IAppCoreResponse<ProjectForAdd[]>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/projects/Create';
        return this.http.post<IAppCoreResponse<ProjectForAdd[]>>(url,projectForCreate);
    }

    public DeleteProject(id):Observable<IAppCoreResponse<any>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/projects/DeleteProject/' + id;
        return this.http.delete<IAppCoreResponse<any>>(url);
    }

    public UploadImg(id:string,data: any):Observable<IAppCoreResponse<any>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/projects/UploadImage/' + id;
        return this.http.post<IAppCoreResponse<any>>(url, data);
    }
}