import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { User } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class HttpService {

  private REST_API_SERVER = 'http://localhost:5001/api';

  private httpOptions = {
    headers: new HttpHeaders({
      'Content-Type': 'application/json'
    })
  };

  constructor(private http: HttpClient) {}

  login(usernameOrEmail: string, password: string): Observable<any> {
    const url = `${this.REST_API_SERVER}/Auth/login`;
    return this.http.post<any>(url, { usernameOrEmail, password }, this.httpOptions);
  }

  register(
    username: string,
    email: string,
    password: string,
    fullName: string,
    phone?: string,
    idCard?: string
  ): Observable<any> {
    const url = `${this.REST_API_SERVER}/Auth/register`;
    return this.http.post<any>(url, {
      username,
      email,
      password,
      fullName,
      phone,
      idCard
    }, this.httpOptions);
  }

  getUsers(): Observable<User[]> {
    const url = `${this.REST_API_SERVER}/Users`;
    return this.http.get<User[]>(url, this.httpOptions);
  }

  createUser(user: User): Observable<User> {
    const url = `${this.REST_API_SERVER}/Users`;
    return this.http.post<User>(url, user, this.httpOptions);
  }
}
