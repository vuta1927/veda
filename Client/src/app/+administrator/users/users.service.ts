import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/Observable';
import { IUser, IUserForCreateOrEdit } from '../../shared/models/user.model';
import { ConfigurationService } from '../../shared/services/configuration.service';
import { IAppCoreResponse } from '../../shared/models/appcore-response.model';

@Injectable()
export class UsersService {

    constructor(private http: HttpClient, private configurationService: ConfigurationService) {}

    public getUsers(params?: any): Observable<IAppCoreResponse<IUser[]>> {
        let url = this.configurationService.serverSettings.apiUrl;
        if(params){
            url += '/api/users/GetUser/';
            return this.http.get<IAppCoreResponse<IUser[]>>(url + params);
        }
        else{
            url += '/api/users/GetUserList';
            return this.http.get<IAppCoreResponse<IUser[]>>(url);
        }
    }
    
    public getById(id: number): Observable<IAppCoreResponse<IUser>>  {
        let url = this.configurationService.serverSettings.apiUrl + '/api/users/' + id;
        return this.http.get<IAppCoreResponse<IUser>>(url);
    }

    public getUserForCreatOrEdit(id?: number): Observable<IAppCoreResponse<IUserForCreateOrEdit>> {
        let url = this.configurationService.serverSettings.apiUrl + '/api/users/getUserForCreateOrEdit/' + (id ? id : -1).toString();
        return this.http.get<IAppCoreResponse<IUserForCreateOrEdit>>(url);
    }

    public getByUsername(username: string): Observable<IAppCoreResponse<IUser>> {
        let url = this.configurationService.serverSettings.apiUrl + '/api/users/withusername/' + username;
        return this.http.get<IAppCoreResponse<IUser>>(url);
    }

    public getByEmail(email: string): Observable<IAppCoreResponse<IUser>> {
        let url = this.configurationService.serverSettings.apiUrl + '/api/users/withemail/' + email;
        return this.http.get<IAppCoreResponse<IUser>>(url);
    }

    public AddUser(user: any): Observable<IAppCoreResponse<IUser>> {
        let url = this.configurationService.serverSettings.apiUrl + '/api/users/AddUser';
        return this.http.post<IAppCoreResponse<IUser>>(url, user);
    }

    public UpdateUser(user: any): Observable<IAppCoreResponse<IUser>> {
        let url = this.configurationService.serverSettings.apiUrl + '/api/users/UpdateUser/' + user.id;
        return this.http.put<IAppCoreResponse<IUser>>(url, user);
    }
}