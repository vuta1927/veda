import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { ConfigurationService } from "../../../shared/services/configuration.service";
import { Observable } from "rxjs/Observable";
import { IAppCoreResponse } from "../../../shared/models/appcore-response.model";
import { PermissionForView, PermissionCategory } from "../../../shared/models/permission.model";

@Injectable()
export class PermissionService {
    constructor(private http: HttpClient, private configurationService: ConfigurationService) {}

    public getPermissions(): Observable<IAppCoreResponse<PermissionForView[]>> {
        let url = this.configurationService.serverSettings.apiUrl + '/api/permissions';

        return this.http.get<IAppCoreResponse<PermissionForView[]>>(url);
    }

    public getCategory(): Observable<IAppCoreResponse<string[]>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/permissions/getcategorys';
        return this.http.get<IAppCoreResponse<string[]>>(url);
    }

    public getPermissionCategory(category: string, roleId: string): Observable<IAppCoreResponse<PermissionCategory[]>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/permissions/GetPermissionByCategory/' + category +'/' + roleId;
        return this.http.get<IAppCoreResponse<PermissionCategory[]>>(url);
    }

    public getAllPermissionCategory(roleId: Number): Observable<IAppCoreResponse<PermissionCategory[]>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/permissions/GetPermissionByCategory/' + roleId;
        return this.http.get<IAppCoreResponse<PermissionCategory[]>>(url);
    }

    public UpdatePermission(permissionForUpdate, roleId:Number):Observable<IAppCoreResponse<PermissionCategory[]>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/permissions/AddOrUpdatePermission/' + roleId;

        return this.http.put<IAppCoreResponse<PermissionCategory[]>>(url,permissionForUpdate);
    }

    public AddPermission(permissionForCreate):Observable<IAppCoreResponse<PermissionForView[]>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/permissions';
        return this.http.post<IAppCoreResponse<PermissionForView[]>>(url,permissionForCreate);
    }

    public DeletePermission(id):Observable<IAppCoreResponse<any>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/permissions/' + id;
        return this.http.delete<IAppCoreResponse<any>>(url);
    }
}