import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { ConfigurationService } from "../../shared/services/configuration.service";
import { Observable } from "rxjs/Observable";
import { IAppCoreResponse } from "../../shared/models/appcore-response.model";
import { Merge } from "../../shared/models/merge.model";
@Injectable()
export class MergeService{
    constructor(private http: HttpClient, private configurationService: ConfigurationService){}

    public mergeProjcet(data: Merge){
        let url = this.configurationService.serverSettings.apiUrl + '/api/Merge/MergeProjcet';
        return this.http.post<IAppCoreResponse<any>>(url, data);
    }
}