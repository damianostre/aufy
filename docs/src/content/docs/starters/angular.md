---
title: Angular Starter
sidebar:
    order: 5
---

## Technology stack

- Angular 18 using latest APIs
- Tailwind CSS

## Custom Sign up flow

To enable custom sign up flow for `Angular` client project, you need to:

* Find `/src/app/auth/auth.routes.ts` file in `Client.Angular` project
* Find the following code:

```tsx
{ path: 'signup', component: SignUpPageComponent },
{ path: 'signup-external', component: SignUpPageComponent },
```
and replace it with:

```tsx
{ path: 'signup', component: SignUpExtendedPageComponent },
{ path: 'signup-external', component: SignUpExternalExtendedPageComponent },
```

There should be already a commented out example of custom sign up flow in `/src/app/auth/auth.routes.ts` file.
    




