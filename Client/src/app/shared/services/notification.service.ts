import { Injectable } from '@angular/core';

declare var $: any;

@Injectable()
export class NotificationService {

    public smallBox(data, cb?) {
        $.smallBox(data, cb);
    }

    public bigBox(data, cb?) {
        $.bigBox(data, cb);
    }

    public smartMessageBox(data, cb?) {
        $.SmartMessageBox(data, cb);
    }

}
