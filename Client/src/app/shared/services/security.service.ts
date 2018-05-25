import { Injectable } from '@angular/core';
import { StorageService } from './storage.service';
import { Router, ActivatedRoute } from '@angular/router';
import { Observable } from 'rxjs/Rx';
import { OAuthService, JwksValidationHandler } from 'angular-oauth2-oidc';
import { authPasswordFlowConfig } from '../../auth.config';
import * as _ from 'lodash';
import { ConfigurationService } from './configuration.service';
import { UtilityService } from './utility.service';

@Injectable()
export class SecurityService {
    constructor(private oauthService: OAuthService, private configurationService: ConfigurationService, private utilService: UtilityService) { }

    public Config() {
        console.log(this.configurationService.serverSettings.identityUrl);
        this.oauthService.configure(authPasswordFlowConfig(this.utilService.stripTrailingSlash(this.configurationService.serverSettings.identityUrl)));
        this.oauthService.setStorage(sessionStorage);
        this.oauthService.tokenValidationHandler = new JwksValidationHandler();
        this.oauthService.loadDiscoveryDocumentAndTryLogin();
        this.oauthService.setupAutomaticSilentRefresh();
    }

    public getClaim(): any {
        let dataAccessToken: any = this.getDataFromToken(this.oauthService.getAccessToken());
        let permissions = dataAccessToken.Permission;
        return permissions;
        // return this.oauthService.getIdentityClaims();
    }

    public getEmail(): any {
        let dataAccessToken: any = this.getDataFromToken(this.oauthService.getAccessToken());
        let email = _.values(dataAccessToken.email);
        return email.join("");
        // return this.oauthService.getIdentityClaims();
    }

    public getUserId(): any {
        let dataAccessToken: any = this.getDataFromToken(this.oauthService.getAccessToken());
        let userId = _.values(dataAccessToken.id);
        return userId.join("");
        // return this.oauthService.getIdentityClaims();
    }

    public getUserRoles(): any {
        let dataAccessToken: any = this.getDataFromToken(this.oauthService.getAccessToken());
        let userRoles = dataAccessToken.Roles;
        return userRoles;
        // return this.oauthService.getIdentityClaims();
    }

    public isInRole(role:string){
        if (!this.IsAuthorized) {
            return false;
        }

        let dataAccessToken: any = this.getDataFromToken(this.oauthService.getAccessToken());
        if(!dataAccessToken.Roles){
            return false;
        }
        if(dataAccessToken.Roles.constructor === Array){
            return _.includes(dataAccessToken.Roles, role);
        }else{
            if(dataAccessToken.Roles == role) return true;
        }
        return false;
        // let roles = _.values(dataAccessToken.Roles);

        // return _.includes(roles, role);
    }

    public get IsAuthorized(): boolean {
        return this.oauthService.hasValidAccessToken();
    }

    public get CurrentUserData(): Observable<Object> {
        return Observable.fromPromise(this.oauthService.loadUserProfile());
    }

    public Login(username: string, password: string): Observable<any> {
        return Observable.fromPromise(this.oauthService.fetchTokenUsingPasswordFlow(username, password));
    }

    public getToken(): any {
        return this.oauthService.getAccessToken();
    }

    public Logoff() {
        this.oauthService.logOut();
    }

    public IsGranted(permission: string): boolean {
        if (!this.IsAuthorized) {
            return false;
        }

        let dataAccessToken: any = this.getDataFromToken(this.oauthService.getAccessToken());
        let permissions = _.values(dataAccessToken.Permission);

        return _.includes(permissions, permission);
    }

    public IsRole(role: string): boolean{
        if(!this.IsAuthorized) return false;
        let dataAccessToken: any = this.getDataFromToken(this.oauthService.getAccessToken());
        let roles = _.values(dataAccessToken.Roles);

        return _.includes(roles, role);
    }

    public isActive(): boolean{
        if(!this.IsAuthorized) return false;
        let dataAccessToken: any = this.getDataFromToken(this.oauthService.getAccessToken());
        let isActive = Boolean(dataAccessToken.active);

        return isActive;
    }

    private urlBase64Decode(str: string) {
        let output = str.replace('-', '+').replace('_', '/');
        switch (output.length % 4) {
            case 0:
                break;
            case 2:
                output += '==';
                break;
            case 3:
                output += '=';
                break;
            default:
                throw 'Illegal base64url string!';
        }

        return window.atob(output);
    }

    private getDataFromToken(token: any) {
        let data = {};
        if (typeof token !== 'undefined') {
            let encoded = token.split('.')[1];
            data = JSON.parse(this.urlBase64Decode(encoded));
        }

        return data;
    }
}