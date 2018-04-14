import { Component, Input, Output, EventEmitter } from "@angular/core";

@Component({
    selector: 'form-alert',
    templateUrl: './form-alert.component.html'
})
export class FormAlertComponent {
    @Input() public type: string = 'danger';
    @Input() public message: string;
    @Output() public close = new EventEmitter();

    closeHandler() { this.close.emit(null); }
}