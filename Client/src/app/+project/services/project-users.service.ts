import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { ConfigurationService } from "../../shared/services/configuration.service";
import { Observable } from "rxjs/Observable";
import { IAppCoreResponse } from "../../shared/models/appcore-response.model";
import { IProjectUser, IProjectUserForAdd } from "../../shared/models/project-user.model";
@Injectable()
export class ProjectUserService{
    constructor(private http: HttpClient, private configurationService: ConfigurationService){}

    public getProjectUsers(id: string, param:string){
        let url = this.configurationService.serverSettings.apiUrl + '/api/ProjectUsers/GetProjectUsers/' + id + '/'+param;
        let result = this.http.get<IAppCoreResponse<IProjectUser[]>>(url)
        return result;
    }
    public getTotal(id: string):Observable<IAppCoreResponse<number>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/ProjectUsers/GetTotal/' + id;
        return this.http.get<IAppCoreResponse<number>>(url);
    }
    public getProjectUserById(id?:string){
        let url = this.configurationService.serverSettings.apiUrl + '/api/ProjectUsers/GetProjectUserById/'+id;

        return this.http.get<IAppCoreResponse<IProjectUser[]>>(url);
    }

    public UpdateProjectUser(ProjectUser):Observable<IAppCoreResponse<IProjectUser[]>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/ProjectUsers/Update';

        return this.http.put<IAppCoreResponse<IProjectUser[]>>(url,ProjectUser);
    }

    public AddProjectUser(ProjectUser):Observable<IAppCoreResponse<any>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/ProjectUsers/AddProjectUser';
        return this.http.post<IAppCoreResponse<any>>(url,ProjectUser);
    }

    public DeleteProjectUser(id):Observable<IAppCoreResponse<any>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/ProjectUsers/DeleteProjectUser/' + id;
        return this.http.delete<IAppCoreResponse<any>>(url);
    }
}