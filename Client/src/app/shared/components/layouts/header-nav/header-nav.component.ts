import { Component, OnInit, AfterViewInit } from '@angular/core';
import { UserProfileService } from '../../../services/userProfile.service';
import { UserProfile } from '../../../models/user.model';
declare let mLayout: any;
@Component({
    selector: "app-header-nav",
    templateUrl: "./header-nav.component.html"
})
export class HeaderNavComponent implements AfterViewInit {
    userProfile: UserProfile = new UserProfile();
    constructor(private userProfileService:UserProfileService){

    }
    ngAfterViewInit() {

        mLayout.initHeader();
        this.userProfileService.getProject().toPromise().then(Response=>{
            if(Response.result){
                this.userProfile = Response.result;
            }
        }).catch(Response=>{
            
        });
    }

}