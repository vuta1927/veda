import { AfterContentInit, OnDestroy, Directive, Input, ContentChild, ElementRef, Renderer2 } from "@angular/core";
import { FormControlName, FormControlDirective, NgModel, FormControl } from "@angular/forms";
import { Subject } from "rxjs";

@Directive({
    selector: '[ngxErrorsContent]',
    exportAs: 'ngxErrorsContent'
})
export class NgxErrorsContentDirective implements AfterContentInit, OnDestroy {

    @Input() checkOnDirty = true;

    @ContentChild(FormControlName) _formCtrlName: FormControlName;
    @ContentChild(FormControlDirective) _formCtrlDir: FormControlDirective;
    @ContentChild(NgModel) _ngModel: NgModel;
    @ContentChild(FormControlName, {read: ElementRef}) _elFormCtrlName: ElementRef;
    @ContentChild(FormControlDirective, {read: ElementRef}) _elFormCtrlDir: ElementRef;
    @ContentChild(NgModel, {read: ElementRef}) _elNgModel: ElementRef;

    onChange$ = new Subject();

    elFormCtrlBlurListener: Function;
    elFormCtrlFocusListener: Function;
    elFormCtrlInputListener: Function;

    constructor(private renderer: Renderer2) {}

    ngAfterContentInit() {
        if (!this.elFormControl) {
            throw new Error(
                '.control-errors-content must be used with a child [formControl], [ngModel] or [formControlName] directive');
        }

        this.initOnChange();
    }

    get invalid(): boolean {
        if (this.formControl) {
            return this.formControl.errors && (!this.checkOnDirty || (this.formControl.dirty || this.formControl.touched));
        } else {
            return false;
        }
    }

    initOnChange() {
        this.elFormCtrlBlurListener = this.renderer.listen(
            this.elFormControl, 'blur', (e) => this.onChange$.next(e));
        this.elFormCtrlFocusListener = this.renderer.listen(
            this.elFormControl, 'focus', (e) => this.onChange$.next(e));
        this.elFormCtrlInputListener = this.renderer.listen(
            this.elFormControl, 'input', (e) => this.onChange$.next(e));
    }

    get formControl(): FormControl | null {
        let _formControl = null;

        if (this._formCtrlName) {
            _formControl = this._formCtrlName;
        } else if (this._formCtrlDir) {
            _formControl = this._formCtrlDir;
        } else if (this._ngModel) {
            _formControl = this._ngModel;
        }

        if (_formControl) {
            _formControl = ('control' in _formControl) ? _formControl.control : null;
        }

        return _formControl;
    }

    get elFormControl(): HTMLElement | null {
        let _elFormControl = null;

        if (this._elFormCtrlName) {
            _elFormControl = this._elFormCtrlName.nativeElement;
        } else if (this._elFormCtrlDir) {
            _elFormControl = this._elFormCtrlDir.nativeElement;
        } else if (this._elNgModel) {
            _elFormControl = this._elNgModel.nativeElement;
        }

        return _elFormControl;
    }

    unbindElFormCtrlBlurListener() {
        if (this.elFormCtrlBlurListener) {
            this.elFormCtrlBlurListener();
            this.elFormCtrlBlurListener = null;
        }
    }

    unbindElFormCtrlFocusListener() {
        if (this.elFormCtrlFocusListener) {
            this.elFormCtrlFocusListener();
            this.elFormCtrlFocusListener = null;
        }
    }

    unbindElFormCtrlInputListener() {
        if (this.elFormCtrlInputListener) {
            this.elFormCtrlInputListener();
            this.elFormCtrlInputListener = null;
        }
    }

    ngOnDestroy() {
        this.unbindElFormCtrlBlurListener();
        this.unbindElFormCtrlFocusListener();
        this.unbindElFormCtrlInputListener();
    }
}