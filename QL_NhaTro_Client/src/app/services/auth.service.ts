import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { HttpService } from './http.service';
import { User } from '../models/user.model';

interface LoginResponse {
  token: string;
  user: User;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private userSubject = new BehaviorSubject<User | null>(null);
  user$ = this.userSubject.asObservable();

  constructor(private http: HttpService) {
    this.loadUser();
  }

  login(usernameOrEmail: string, password: string): Observable<LoginResponse> {
    return this.http.login(usernameOrEmail, password).pipe(
      tap(res => this.setSession(res))
    );
  }

  register(
    username: string,
    email: string,
    password: string,
    fullName: string,
    phone?: string,
    idCard?: string
  ) {
    return this.http.register(username, email, password, fullName, phone, idCard);
  }

  logout(): void {
    localStorage.clear();
    this.userSubject.next(null);
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }

  getUser(): User | null {
    return this.userSubject.value;
  }

  isAdmin(): boolean {
    const role = this.getUser()?.role?.toLowerCase();
    return role === 'admin';
  }

  isUser(): boolean {
    const role = this.getUser()?.role?.toLowerCase();
    return role === 'user';
  }


  // Deprecated: Use isUser() instead
  isTenant(): boolean {
    return this.isUser();
  }

  private setSession(res: LoginResponse) {
    localStorage.setItem('token', res.token);
    localStorage.setItem('user', JSON.stringify(res.user));
    this.userSubject.next(res.user);
  }

  private loadUser() {
    const user = localStorage.getItem('user');
    if (user) {
      this.userSubject.next(JSON.parse(user));
    }
  }
}
