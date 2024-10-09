---
title: Vue.js Starter
sidebar:
    order: 4
---

## Technology stack

- TypeScript
- Vite for the build system
- Vue 3
- Zod for form validation
- Axios for HTTP requests
- Tailwind CSS

## Custom Sign up flow

To enable custom sign up flow for `Vue` client project, you need to:

* Find `/src/router/index.ts` file in `Client.Vue` project
* Find the following code:

```typescript
{ path: '/signup', component: SignUpView, name: 'signup' },
{ path: '/signup-external', component: SignUpExternalView, name: 'signup-external' },
```
and replace it with:

```typescript
{ path: '/signup', component: SignUpExtendedView },
{ path: '/signup-external', component: SignUpExternalExtendedView },
```

There should be already a commented out example of custom sign up flow in `/src/router/index.ts` file.


    




