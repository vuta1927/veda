import { Pipe, PipeTransform } from '@angular/core';

@Pipe({name: 'customQcStatus'})
export class CustomQcStatus implements PipeTransform {
  transform(value: string): string {
      if(!value) return;
    let status: string[] = value.split(';');
    var newStr: string = '';
    for (var i = 0; i < status.length; i++) {
        if(i ==(status.length - 1)){
            continue;
        }
        if(status[i] == 'passed'){
            newStr += `<span class="m-badge m-badge--success m-badge--wide">${status[i]}<span>`;
        }else{
            newStr += `<span class="m-badge m-badge--error m-badge--wide">${status[i]}<span>`;
        }
    }
    return newStr;
  }
}