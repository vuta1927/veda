import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders, HttpHeaderResponse } from "@angular/common/http";
import { ResponseContentType } from '@angular/http';
import { ConfigurationService } from "../../shared/services/configuration.service";
import { Observable } from "rxjs/Observable";
import { IAppCoreResponse } from "../../shared/models/appcore-response.model";
import { SecurityService } from '../../shared/services/security.service';
@Injectable()
export class ImportExportService {
    constructor(
        private http: HttpClient,
        private configurationService: ConfigurationService,
        private auth: SecurityService
    ) { }

    public Import(projectId: string, data: any): Observable<IAppCoreResponse<any>> {
        let url = this.configurationService.serverSettings.apiUrl + '/api/ImportProject/' + projectId;
        return this.http.post<IAppCoreResponse<any>>(url, data);
    }

    public Export(projectId: string, data: any) {
        const token = this.auth.getToken();
        const httpOptions = {
            headers: new HttpHeaders({
                'Content-Type': 'application/json',
                'Accept': 'application/zip',
                'Authorization': token
            }),
            responseType:'arraybuffer' as 'arraybuffer'
        }

        let url = this.configurationService.serverSettings.apiUrl + '/api/ExportProject/' + projectId;
        return this.http.post(url, data, httpOptions);
    }

}