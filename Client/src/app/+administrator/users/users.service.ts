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
            url += '/api/Users/GetUser/';
            return this.http.get<IAppCoreResponse<IUser[]>>(url + params);
        }
        else{
            url += '/api/Users/GetUserList';
            return this.http.get<IAppCoreResponse<IUser[]>>(url);
        }
    }
    
    public getById(id: number): Observable<IAppCoreResponse<IUser>>  {
        let url = this.configurationService.serverSettings.apiUrl + '/api/Users/' + id;
        return this.http.get<IAppCoreResponse<IUser>>(url);
    }

    public getUserForCreatOrEdit(id?: number): Observable<IAppCoreResponse<IUserForCreateOrEdit>> {
        let url = this.configurationService.serverSettings.apiUrl + '/api/Users/getUserForCreateOrEdit/' + (id ? id : -1).toString();
        return this.http.get<IAppCoreResponse<IUserForCreateOrEdit>>(url);
    }

    public getByUsername(username: string): Observable<IAppCoreResponse<IUser>> {
        let url = this.configurationService.serverSettings.apiUrl + '/api/Users/withusername/' + username;
        return this.http.get<IAppCoreResponse<IUser>>(url);
    }

    public getByEmail(email: string): Observable<IAppCoreResponse<IUser>> {
        let url = this.configurationService.serverSettings.apiUrl + '/api/Users/withemail/' + email;
        return this.http.get<IAppCoreResponse<IUser>>(url);
    }

    public AddUser(user: any): Observable<IAppCoreResponse<IUser>> {
        let url = this.configurationService.serverSettings.apiUrl + '/api/Users/AddOrUpdateUser';
        return this.http.post<IAppCoreResponse<IUser>>(url, user);
    }

    public UpdateUser(user: any): Observable<IAppCoreResponse<IUser>> {
        let url = this.configurationService.serverSettings.apiUrl + '/api/Users/UpdateUser/' + user.id;
        return this.http.put<IAppCoreResponse<IUser>>(url, user);
    }
}