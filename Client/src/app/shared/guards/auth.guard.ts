import { OnDestroy, Injectable } from '@angular/core';
import {
    ActivatedRouteSnapshot,
    CanActivateChild,
    CanLoad,
    Router,
    RouterStateSnapshot,
    CanActivate,
    Route
} from '@angular/router';
import { SecurityService } from '../services/security.service';
@Injectable()
export class AuthGuard implements CanActivate, CanActivateChild, CanLoad {

    constructor(private securityService: SecurityService, private router: Router) {}

    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
        const url: string = state.url;
        return this.checkLogin(url);
    }

    public canActivateChild(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
        return this.canActivate(route, state);
    }

    public canLoad(route: Route): boolean {
        const url = `/${route.path}`;
        return this.checkLogin(url);
    }

    private checkLogin(url: string): boolean {
        if (this.securityService.IsAuthorized) {
            return true;
        }

        this.router.navigate(['/account/login'], {queryParams: { returnUrl: url }});

        return false;
    }
}

@Injectable()
export class ProjectGuard implements CanActivate, CanLoad {
    constructor(private router: Router, private securityService: SecurityService) {}
    viewProject: string = 'ViewProject';
    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
        // const url: string = state.url;
        return this.checkPermission();
    }

    public canLoad(route: Route): boolean {
        // const url = `/${route.path}`;
        return this.checkPermission();
    }

    private checkPermission(): boolean {
        var claims = this.securityService.getClaim();
        // console.log(claims);
        if(!claims) return false;
        if(claims.indexOf(this.viewProject) > -1){
            return true;
        }
        return false;
    }
}

@Injectable()
export class UserGuard implements CanActivate {
    constructor(private router: Router, private securityService: SecurityService) {}
    ViewUser:string = 'ViewUser';
    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
        // const url: string = state.url;
        return this.checkPermission();
    }

    private checkPermission(): boolean {
        var claims = this.securityService.getClaim();
        // console.log(claims);
        if(!claims) return false;
        if(claims.indexOf(this.ViewUser) > -1){
            return true;
        }
        return false;
    }
}

@Injectable()
export class RoleGuard implements CanActivate {
    constructor(private router: Router, private securityService: SecurityService) {}
    ViewRole:string = 'ViewRole';
    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
        // const url: string = state.url;
        return this.checkPermission();
    }

    private checkPermission(): boolean {
        var claims = this.securityService.getClaim();
        // console.log(claims);
        if(!claims) return false;
        if(claims.indexOf(this.ViewRole) > -1){
            return true;
        }
        return false;
    }
}

@Injectable()
export class AdminGuard implements CanActivate {
    constructor(private router: Router, private securityService: SecurityService) {}
    admin: string = 'Administrator';
    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
        // const url: string = state.url;
        return this.checkPermission();
    }

    private checkPermission(): boolean {
        if(this.securityService.isInRole(this.admin)){
            return true;
        }else{
            return false;
        }
        // var claims = this.securityService.getClaim();
        // if(!claims) return false;
        // if(claims.indexOf(this.admin) > -1){
        //     return true;
        // }
        // return false;
    }
}