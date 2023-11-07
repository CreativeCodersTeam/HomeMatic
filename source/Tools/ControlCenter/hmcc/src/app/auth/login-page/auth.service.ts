import { Injectable } from '@angular/core';
import {HttpClient} from "@angular/common/http";
import * as moment from "moment/moment";
import {shareReplay, tap} from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(private http: HttpClient) {

  }

  // login(email:string, password:string ) {
  //   return this.http.post('/api/login', {email, password})
  //     .pipe(tap(res => this.setSession), shareReplay(1));
  // }
  //
  // private setSession(authResult) {
  //   const expiresAt = moment().add(authResult.authToken, 'second');
  //
  //   localStorage.setItem('id_token', authResult.authToken);
  //   localStorage.setItem("expires_at", JSON.stringify(expiresAt.valueOf()) );
  // }
  //
  // logout() {
  //   localStorage.removeItem("id_token");
  //   localStorage.removeItem("expires_at");
  // }
  //
  // public isLoggedIn() {
  //   return moment().isBefore(this.getExpiration());
  // }
  //
  // isLoggedOut() {
  //   return !this.isLoggedIn();
  // }
  //
  // getExpiration() {
  //   const expiration = localStorage.getItem("expires_at");
  //
  //   if (!expiration) {
  //     return moment(0);
  //   }
  //
  //   const expiresAt = JSON.parse(expiration);
  //   return moment(expiresAt);
  // }

  // public login(username: string, password: string) {
  //   return true;
  // }
}
