import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { ConfigurationService } from "../../shared/services/configuration.service";
import { Observable } from "rxjs/Observable";
import { IAppCoreResponse } from "../../shared/models/appcore-response.model";
@Injectable()
export class ExportExportService{
    constructor(private http: HttpClient, private configurationService: ConfigurationService){}

    public Export(projectId:string,data: any):Observable<IAppCoreResponse<any>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/ExportProject/' + projectId;
        return this.http.post<IAppCoreResponse<any>>(url, data);
    }
}