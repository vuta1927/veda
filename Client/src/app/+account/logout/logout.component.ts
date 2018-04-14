import { OnInit, Component } from "@angular/core";
import { Router } from "@angular/router";
import { SecurityService } from "../../shared/services/security.service";
import { Helpers } from "../../helpers";

@Component({
    selector: 'app-logout',
    template: ''
})
export class LogoutComponent implements OnInit {

    constructor(
        private router: Router,
        private _securityService: SecurityService) { }

    ngOnInit() {
        Helpers.setLoading(true);
        this._securityService.Logoff();
        this.router.navigate(['account/login']);
    }
}