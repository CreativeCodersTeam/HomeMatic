import { Injectable } from '@angular/core';
import {Observable, Subject} from "rxjs";
import {Message} from "./message";

@Injectable({
  providedIn: 'root'
})
export class MessengerService {

  private messageSubject = new Subject<Message>();

  sendMessage(message: Message) {
    this.messageSubject.next(message);
  }

  get messages() : Observable<Message>{
    return this.messageSubject;
  }
}
