import { Component, OnInit, AfterViewInit } from '@angular/core';

declare let mLayout: any;
@Component({
    selector: "app-header-nav",
    templateUrl: "./header-nav.component.html"
})
export class HeaderNavComponent implements AfterViewInit {

    ngAfterViewInit() {

        mLayout.initHeader();

    }

}