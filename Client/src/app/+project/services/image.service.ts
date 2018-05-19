import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { ConfigurationService } from "../../shared/services/configuration.service";
import { Observable } from "rxjs/Observable";
import { IAppCoreResponse } from "../../shared/models/appcore-response.model";
import { Iimage } from "../../shared/models/image.model";
@Injectable()
export class ImageService{
    constructor(private http: HttpClient, private configurationService: ConfigurationService){}

    public getImages(id: string, param:string){
        let url = this.configurationService.serverSettings.apiUrl + '/api/images/GetImage/' + id + '/'+param;
        let result = this.http.get<IAppCoreResponse<Iimage[]>>(url)
        return result;
    }

    public getTotal(projectId:string):Observable<IAppCoreResponse<number>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/images/GetTotal/' + projectId;
        return this.http.get<IAppCoreResponse<number>>(url);
    }

    public getImageListId(projectId: string):Observable<IAppCoreResponse<any>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/images/GetImageListId/' + projectId;
        return this.http.get<IAppCoreResponse<any>>(url);
    }

    public getImageBinary(imgId: string, projId:string):Observable<Blob>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/images/GetImageBinary/' + imgId + '/'+ projId;
        return this.http.get(url, { responseType: 'blob'});
    }

    public getImageById(userId: number, projectId:string, imageId:string){
        let url = this.configurationService.serverSettings.apiUrl + '/api/images/GetImageById/'+ userId +'/'+projectId+'/' + imageId;

        return this.http.get<IAppCoreResponse<any>>(url);
    }

    public getNextImage(userId: number, projectId:string, imageId:string):Observable<IAppCoreResponse<any>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/images/GetNextImage/'+ userId +'/'+projectId+'/' + imageId;

        return this.http.get<IAppCoreResponse<any>>(url);
    }

    public sendPing(projectId:string, imgId:string):Observable<IAppCoreResponse<any>>{
        let url = this.configurationService.serverSettings.apiUrl +'/api/images/Ping/'+projectId+'/'+imgId;
        return this.http.post<IAppCoreResponse<any>>(url, null);
    }

    public getCurrentWorker(projectId:string, imageId:string, userId:number):Observable<IAppCoreResponse<Iimage>>{
        let url = this.configurationService.serverSettings.apiUrl +'/api/images/GetCurrentWorker/'+projectId+'/'+imageId+'/'+userId;
        return this.http.get<IAppCoreResponse<Iimage>>(url);
    }

    public relaseImage(userId:number,projId:string, imgId:string):Observable<IAppCoreResponse<any>>{
        let url = this.configurationService.serverSettings.apiUrl +'/api/images/ReleaseImage/'+ userId +'/'+ projId +'/'+imgId;
        return this.http.delete<IAppCoreResponse<any>>(url);
    }

    public UpdateImage(image):Observable<IAppCoreResponse<Iimage[]>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/images/update';

        return this.http.put<IAppCoreResponse<Iimage[]>>(url,image);
    }

    public updateTaggedTime(imageId:string, taggedTime: number):Observable<IAppCoreResponse<any>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/images/UpdateTaggedTime/' + imageId;

        return this.http.put<IAppCoreResponse<any>>(url,taggedTime);
    }

    public AddImage(image):Observable<IAppCoreResponse<Iimage[]>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/images/AddImage';
        return this.http.post<IAppCoreResponse<Iimage[]>>(url,image);
    }

    public DeleteImage(id):Observable<IAppCoreResponse<any>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/images/DeleteImage/' + id;
        return this.http.delete<IAppCoreResponse<any>>(url);
    }
}