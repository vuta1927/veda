import { Component, ChangeDetectionStrategy, OnDestroy, AfterContentInit, Input, ViewContainerRef, Optional, Host } from "@angular/core";
import { AbstractControl, ControlContainer } from "@angular/forms";
import { Subscription } from "rxjs";
import { filter } from "rxjs/operator/filter";
import { NgxDisplayContext, NgxErrorsContext } from "./ngx-errors.model";
import { NgxErrorsService } from "./ngx-errors.service";
import { NgxErrorsContentDirective } from "./ngx-errors-content.directive";

@Component({
    selector: '[ngxErrors]',
    template: '',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class NgxErrorsComponent implements OnDestroy, AfterContentInit {


    @Input()
    set maxError(value: number) {
        this.context.maxError = value;
    }

    @Input('ngxErrors')
    set ngxErrors(value: AbstractControl | string) {
        if (value !== this._ngxErrors) {
            this.destroy();

            if (typeof value === 'string') {
                this._ngxErrors = this.control.control.get(value);
                if (!this._ngxErrors) {

                    return;
                }
            }

            if (this._ngxErrors) {
                const s = this._ngxErrors.statusChanges.subscribe(o => this.update());
                this.unsubscribe.push(s);
                this.update();
            }
        }
    }

    @Input()
    set exclude(value: string[]) {
        if (this._exclude !== value) {
            this._exclude = Array.isArray(value) ? value : [];
            this.update();
        }
    }

    private _exclude: string[] = [];
    private _ngxErrors: AbstractControl;
    private unsubscribe: Subscription[] = [];
    private ready: boolean;
    private context: NgxDisplayContext = {} as any;

    constructor(
        private ngxErrorsService: NgxErrorsService,
        private control: ControlContainer,
        private vcr: ViewContainerRef,
        @Optional() @Host() private ngxContent: NgxErrorsContentDirective
    ) { 
        if (!this.ngxContent) {
            throw new Error(
                'control-errors must be used with a parent ngxErrorsContent directive');
        }
    }

    get invalid() { return this.ngxContent.invalid; }

    ngAfterContentInit() {
        Object.defineProperty(this, 'ready', { value: true });

        /*  Create the context.
            The context is determined by priority based on hierarchy, the order (high -> low):
            - Local template defined as content
            - Default SCOPED template (NgxErrorTemplate defined within the ControlContainer)
            - Global Default template
        */
        const context = this.ngxErrorsService.getContextStore(this, this.control).get();

        // Now copy values from the context to the local context, skipping values that already exists
        // on the local context.
        Object.keys(context).forEach(k => {
            if (!this.context[k]) {
                this.context[k] = context[k];
            }
        });

        if (!this.context.maxError) {
            this.context.maxError = Number.POSITIVE_INFINITY;
        }

        // setTimeout(() => this.update(), 16)
        const s = this.ngxContent.onChange$.subscribe(() => this.update());
        this.update();
        this.unsubscribe.push(s);
    }

    ngOnDestroy() {
        this.ngxErrorsService.removeScope(this);
        this.destroy();
    }

    private hasControl(value: string): boolean {
        return !!this.control.control.get(value);
    }

    private addToWaitList(value: string): void {
        const unsub = filter.call(this.control.control.valueChanges, () => this.hasControl(value))
            .subscribe(obj => this.ngxErrors = value);
        this.unsubscribe.push(unsub);
    }

    private update(): void {
        if (this.ready) {
            this.vcr.clear();
            if (this._ngxErrors && this._ngxErrors.errors) {

                /* TODO:
                    `this._ngxErrors.errors` contains errors for the current controller without taking into
                    account errors of child controllers.
                    Using this._ngxErrors.invalid will reflect the full error state.
                    Explore supporting this feature.
                */
                const contextStore = this.ngxErrorsService.getContextStore(this, this.control);

                const errors = this._ngxErrors.errors;
                const overrids = contextStore.mergedKeys();

                const errorKeys = Object.keys(errors).filter(key => this._exclude.indexOf(key) === -1);

                const { maxError } = this.context;

                if (maxError > 0 && maxError < errorKeys.length) {
                    errorKeys.splice(maxError, errorKeys.length - maxError);
                }

                if (!(this._ngxErrors.touched || this._ngxErrors.dirty))
                {
                    return;
                }
                
                errorKeys.forEach(name => {
                    const template = overrids[name];
                    
                    const item = {
                        name,
                        message: this.ngxErrorsService.getDefaultMessage(name, errors[name])
                    };

                    this.vcr.createEmbeddedView(
                        template ? template.template : this.context.template,
                        { $implicit: item }
                    );
                })
            }
        }
    }

    private destroy(): void {
        while (this.unsubscribe.length > 0) {
            const s = this.unsubscribe.pop();
            if (!s.closed) {
                s.unsubscribe();
            }
        }
    }
}