import { Injectable } from '@angular/core';
import { Observable, Subscription } from 'rxjs/Rx';

@Injectable()

export class TimeService{
    constructor(){}
    private _ticks = 0;
    timerStart: boolean= false;
    minutesDisplay: number = 0;
    hoursDisplay: number = 0;
    secondsDisplay: number = 0;

    sub: Subscription;

    public startTimer(ticks) {
        if(!this.timerStart){
            this.timerStart = true;
        }else{
            return;
        }
        this._ticks = ticks;
        let timer = Observable.timer(1, 1000);
        this.sub = timer.subscribe(
            t => {
                this._ticks = t;
                this.secondsDisplay = this.getSeconds(this._ticks);
                this.minutesDisplay = this.getMinutes(this._ticks);
                this.hoursDisplay = this.getHours(this._ticks);
            }
        );
    }

    public getTotalTime(){
        return this._ticks;
    }
    public getSeconds(ticks: number) {
        return this.pad(ticks % 60);
    }

    public getMinutes(ticks: number) {
         return this.pad((Math.floor(ticks / 60)) % 60);
    }

    public getHours(ticks: number) {
        return this.pad(Math.floor((ticks / 60) / 60));
    }

    public pad(digit: any) { 
        return digit <= 9 ? '0' + digit : digit;
    }
}