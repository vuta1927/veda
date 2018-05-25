import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { ConfigurationService } from "./configuration.service";
import { Observable } from "rxjs/Observable";
import { IAppCoreResponse } from "../models/appcore-response.model";
import { IUserProfile, UserProfile } from "../models/user.model";
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
@Injectable()
export class UserProfileService {
    private userProfileSource = new BehaviorSubject<UserProfile>( new UserProfile());
    currentUserProfile = this.userProfileSource.asObservable();

    constructor(private http: HttpClient, private configurationService: ConfigurationService) {}

    public changeUserProfile(user: UserProfile){
        this.userProfileSource.next(user);
    }

    public getProject():Observable<IAppCoreResponse<IUserProfile>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/UserProfile';
        return this.http.get<IAppCoreResponse<IUserProfile>>(url);
    }

    public getByUsername(username:string):Observable<IAppCoreResponse<any>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/UserProfile/withusername/'+username;
        return this.http.get<IAppCoreResponse<IUserProfile>>(url);
    }

    public getByEmail(email:string):Observable<IAppCoreResponse<any>>{
        let url = this.configurationService.serverSettings.apiUrl + '/api/UserProfile/withemail/'+email;
        return this.http.get<IAppCoreResponse<IUserProfile>>(url);
    }
}