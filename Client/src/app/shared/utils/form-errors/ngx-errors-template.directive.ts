import { OnInit, OnDestroy, Directive, TemplateRef, Input, Optional } from "@angular/core";
import { NgxErrorsContext, NgxDisplayContext } from "./ngx-errors.model";
import { NgxErrorsComponent } from "./ngx-errors.component";
import { ControlContainer } from "@angular/forms";
import { NgxErrorsService } from "./ngx-errors.service";

@Directive({
    selector: '[ngxErrorTemplate]'
})
export class NgxErrorTemplateDirective implements NgxDisplayContext, OnInit, OnDestroy {
    template: TemplateRef<NgxErrorsContext>;

    @Input('ngxErrorTemplateMaxError') maxError: number;
    @Input('ngxErrorTemplateErrorKey') errorKey: string | undefined;

    constructor(
        template: TemplateRef<NgxErrorsContext>,
        private ngxErrorsService: NgxErrorsService,
        @Optional() private local?: NgxErrorsComponent,
        @Optional() private control?: ControlContainer
    ){
        this.template = template;
    }

    ngOnInit(): void {
        if (this.local) {
            this.ngxErrorsService.setContext(this, this.local, this.control);
        } else {
            this.ngxErrorsService.setContext(this, this.control);
        }
    }

    ngOnDestroy(): void {
        this.ngxErrorsService.removeContext(this, this.local || this.control);
    }
}