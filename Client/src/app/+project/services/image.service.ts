import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { ConfigurationService } from "../../shared/services/configuration.service";
import { Observable } from "rxjs/Observable";
import { IAppCoreResponse } from "../../shared/models/appcore-response.model";
import { Iimage } from "../../shared/models/image.model";
@Injectable()
export class ImageService{
    constructor(private http: HttpClient, private configurationService: ConfigurationService){}

    public getImages(id: string){
        let url = this.configurationService.serverSettings.apiUrl + '/api/images/GetImage/' + id;
        let result = this.http.get<IAppCoreResponse<Iimage[]>>(url)
        return result;
    }

    public getImageById(id?:string){
        let url = this.configurationService.serverSettings.apiUrl + '/api/images/GetImageById/'+id;

        return this.http.get<IAppCoreResponse<Iimage[]>>(url);
    }

    public UpdateImage(image):Observable<IAppCoreResponse<Iimage[]>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/images/update';

        return this.http.put<IAppCoreResponse<Iimage[]>>(url,image);
    }

    public AddImage(image):Observable<IAppCoreResponse<Iimage[]>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/images/AddImage';
        return this.http.post<IAppCoreResponse<Iimage[]>>(url,image);
    }

    public DeleteProject(id):Observable<IAppCoreResponse<any>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/images/DeleteImage/' + id;
        return this.http.delete<IAppCoreResponse<any>>(url);
    }
}