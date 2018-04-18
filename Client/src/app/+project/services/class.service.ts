import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { ConfigurationService } from "../../shared/services/configuration.service";
import { Observable } from "rxjs/Observable";
import { IAppCoreResponse } from "../../shared/models/appcore-response.model";
import { IClass } from "../../shared/models/class.model";
@Injectable()
export class ClassService{
    constructor(private http: HttpClient, private configurationService: ConfigurationService){}

    public getClasses(projectId: string){
        let url = this.configurationService.serverSettings.apiUrl + '/api/Classes/GetClasses/' + projectId ;
        let result = this.http.get<IAppCoreResponse<IClass[]>>(url)
        return result;
    }

    public getClassById(id:number):Observable<IAppCoreResponse<IClass>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/Classes/GetClassById/'+id;

        return this.http.get<IAppCoreResponse<IClass>>(url);
    }

    public getClassByName(id:string, name: string):Observable<IAppCoreResponse<IClass>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/Classes/GetClassByName/'+id+'/'+name;

        return this.http.get<IAppCoreResponse<IClass>>(url);
    }

    public getCodeOfClass(id:string, code: string):Observable<IAppCoreResponse<IClass>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/Classes/GetCodeOfClass/'+id+'/'+code;

        return this.http.get<IAppCoreResponse<IClass>>(url);
    }

    public UpdateClass(id:number, Class):Observable<IAppCoreResponse<IClass>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/Classes/Update/' + id;

        return this.http.put<IAppCoreResponse<IClass>>(url,Class);
    }

    public AddClass(Class):Observable<IAppCoreResponse<IClass>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/Classes/AddClass';
        return this.http.post<IAppCoreResponse<IClass>>(url,Class);
    }

    public DeleteClass(id):Observable<IAppCoreResponse<any>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/Classes/DeleteClass/' + id;
        return this.http.delete<IAppCoreResponse<any>>(url);
    }
}