import {Injectable} from '@angular/core';
import {HttpClient} from "@angular/common/http";
import {catchError, map, of, shareReplay} from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(private http: HttpClient) {

  }

  login(email: string, password: string) {
    return this.http
      .post('/api/tokenauth/login', {userName: email, password: password})
      .pipe(
        map(() => true),
        catchError(() => of(false)),
        shareReplay(1));
  }

  logout() {
    return this.http
      .post('/api/tokenauth/logout', {})
      .pipe(
        map(() => true),
        catchError(() => of(false)),
        shareReplay(1));
  }

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
}
