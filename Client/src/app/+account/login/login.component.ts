import { Component, OnInit, ViewEncapsulation, AfterViewInit } from '@angular/core';
import { SecurityService } from '../../shared/services/security.service';
import { UtilityService } from '../../shared/services/utility.service';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { FormService } from '../../shared/services/form.service';
import { ScriptLoaderService } from '../../shared/services/script-loader.service';
import { Helpers } from '../../helpers';

@Component({
    selector: '.m-grid.m-grid--hor.m-grid--root.m-page',
    templateUrl: './login.component.html',
    encapsulation: ViewEncapsulation.None
})
export class LoginComponent implements OnInit, AfterViewInit {

    public isLoggingin: boolean = false;
    loginForm: FormGroup;
    public isLoginFail: boolean = false;

    constructor(
        private service: SecurityService,
        private utilityService: UtilityService,
        private formBuilder: FormBuilder,
        private formService: FormService,
        private _script: ScriptLoaderService
    ) {
        if (service.IsAuthorized) {
            utilityService.navigateToReturnUrl();
        }

        this.createLoginForm();
    }

    ngOnInit() {
        this._script.loadScripts('body', [
            'assets/vendors/base/vendors.bundle.js',
            'assets/demo/default/base/scripts.bundle.js'
        ], true).then(() => {
            Helpers.setLoading(false);
        })
    }

    ngAfterViewInit() {
        // this._script.loadScripts('.m-grid.m-grid--hor.m-grid--root.m-page', ['assets/snippets/pages/user/login.js']);
        Helpers.bodyClass('m--skin- m-header--fixed m-header--fixed-mobile m-aside-left--enabled m-aside-left--skin-dark m-aside-left--offcanvas m-footer--push m-aside--offcanvas-default');
    }

    createLoginForm() {
        this.loginForm = this.formBuilder.group(
            {
                username: ['', Validators.required],
                password: ['', Validators.required]
            })
    }

    public login(): void {
        if (!this.loginForm.valid) {
            this.formService.validateAllFormFields(this.loginForm);
            return;
        }

        this.isLoggingin = true;
        this.isLoginFail = false;

        this.service.Login(this.loginForm.value.username, this.loginForm.value.password)
            .subscribe(() => {
                this.isLoggingin = false;
                this.utilityService.navigateToReturnUrl();
            }, err => {
                this.isLoggingin = false;
                this.isLoginFail = true;
            });
    }
}