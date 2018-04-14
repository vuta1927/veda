import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Subject';

@Injectable()
export class SpinnerService {
  /** Progress state */
  public state = new Subject<boolean>();
  private _isSpinning: boolean = false;
  private _isEnable: boolean = true;

  /** Start spinning */
  public start() {
    if (this.isSpinning()) {
      return;
    }
    if (!this.isEnable) {
      return;
    }
    this._isSpinning = true;
    this.state.next(true);
  }

  /** Stop spinning */
  public stop() {
    if (!this.isSpinning()) {
      return;
    }
    this._isSpinning = false;
    /** if spinning stop it */
    this.state.next(false);
  }

  public isSpinning() {
    return this._isSpinning;
  }

  get isEnable(): boolean {
    return this._isEnable;
  }

  set isEnable(value: boolean) {
    this._isEnable = value;
  }
}
