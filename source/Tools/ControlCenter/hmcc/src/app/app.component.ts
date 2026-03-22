import { Component } from '@angular/core';
import {MessengerService} from "./shared/messenger.service";
import {filter} from "rxjs";

@Component({
  selector: 'hmcc-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  caption = '';

  constructor(messenger: MessengerService) {
    messenger.messages
      .pipe(filter(x => x.id == 'app-caption'))
      .subscribe(msg => this.caption = msg.data || '');
  }
}
