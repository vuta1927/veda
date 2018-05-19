import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { ConfigurationService } from "../../shared/services/configuration.service";
import { Observable } from "rxjs/Observable";
import { IAppCoreResponse } from "../../shared/models/appcore-response.model";
import { ProjectSetting, ProjectSettingForUpdate } from '../../shared/models/project-setting.model';
@Injectable()
export class ProjectSettingService{
    constructor(private http: HttpClient, private configurationService: ConfigurationService){}

    public getProjects():Observable<IAppCoreResponse<any>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/Projects/GetProjectNames';
        return this.http.get<IAppCoreResponse<any>>(url);
    }

    public getSetting(projectId:string):Observable<IAppCoreResponse<ProjectSetting>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/ProjectSettings/GetProjectSetting/' + projectId;
        return this.http.get<IAppCoreResponse<ProjectSetting>>(url);
    }

    public Update(id:string, data: ProjectSettingForUpdate):Observable<IAppCoreResponse<ProjectSetting>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/ProjectSettings/PutProjectSetting/' + id;
        return this.http.put<IAppCoreResponse<ProjectSetting>>(url, data);
    }
}