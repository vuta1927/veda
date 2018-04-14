import { ErrorHandler, Injectable } from '@angular/core';

@Injectable()
export class GlobalErrorHandler extends ErrorHandler {

    public handleError(errorResponse: any): void {
        if (errorResponse.status === 401) {
            // Unauthorized
        } else if (errorResponse.status === 400) {
            // Notfound
        }

        console.log(errorResponse);
    }
}