# Getting Started with AuthenticationOAuth2Google

This project was created for educational purposes only.

In order to authenticate a User using Google `accessToken` please refer to this repo [Google Auth using React](https://github.com/DCarmona18/oli-chat-react).

The flow consist:
- Frontend sends the `accessToken` retrieved from the login (using Firebase).
- This token will be used as `Bearer` in every single request to the backend.
- We created a new AuthenticationHandler (`FirebaseAuthenticationHandler`) to handle the request and validate against the Firebase service if the token is valid and get the claims.
- It can be extended to add more claims based on the need.

## Considerations
Below example adds the scheme mentioned before
```cs
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddScheme<AuthenticationSchemeOptions, FirebaseAuthenticationHandler>(JwtBearerDefaults.AuthenticationScheme, (o) => { });
```
Since we are using `FirebaseApp` we have to add to the root of our project a file named `firebase-config.json` which is the service account file downloaded in the Firebase dashboard.
```cs
builder.Services.AddSingleton(FirebaseApp.Create());
```

For more info [Oficial Firebase documentation](https://firebase.google.com/docs/admin/setup#c).
