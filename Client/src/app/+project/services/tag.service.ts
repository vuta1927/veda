import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { ConfigurationService } from "../../shared/services/configuration.service";
import { Observable } from "rxjs/Observable";
import { IAppCoreResponse } from "../../shared/models/appcore-response.model";
import { ITag, ITagForAdd, ITagForUpdate, DataUpdate } from "../../shared/models/tag.model";
@Injectable()
export class TagService{
    constructor(private http: HttpClient, private configurationService: ConfigurationService){}

    public getTags(imageId: string){
        let url = this.configurationService.serverSettings.apiUrl + '/api/Tags/GetTags/' + imageId;
        return this.http.get<IAppCoreResponse<ITag[]>>(url);
    }

    public getTagById(id:number):Observable<IAppCoreResponse<ITag>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/Tags/GetTagById/'+id;

        return this.http.get<IAppCoreResponse<ITag>>(url);
    }

    public saveTags(projectId:string, imageId:string, data: DataUpdate):Observable<IAppCoreResponse<any>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/Tags/Update/'+ projectId +'/'+ imageId;
        return this.http.post<IAppCoreResponse<any>>(url, data);
    }

    // public UpdateTag(id:number, Tag):Observable<IAppCoreResponse<ITag>>{
    //     let url = this.configurationService.serverSettings.apiUrl + '/api/Tags/Update/' + id;

    //     return this.http.put<IAppCoreResponse<ITag>>(url,Tag);
    // }

    public AddTag(Tag):Observable<IAppCoreResponse<ITag>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/Tags/AddTag';
        return this.http.post<IAppCoreResponse<ITag>>(url,Tag);
    }

    public DeleteTag(id):Observable<IAppCoreResponse<any>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/Tags/DeleteTag/' + id;
        return this.http.delete<IAppCoreResponse<any>>(url);
    }
}