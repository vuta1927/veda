import { Component, Input } from "@angular/core";

@Component({
    selector: 'breadcrumbs',
    templateUrl: './breadcrumbs.component.html'
})
export class BreadcrumbsComponent {
    
    @Input() public icon: string;
    @Input() public items: Array<string>;
}