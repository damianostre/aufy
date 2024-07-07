---
title: React Starter
sidebar:
    order: 3
---


## Technology stack

- TypeScript
- Vite for the build system
- React router for routing
- React hook form for form handling
- Zod for form validation
- Axios for HTTP requests
- Tailwind CSS for styling

## Custom Sign up flow

To enable custom sign up flow for `React` client project, you need to:

* Find `/src/routes/index.tsx` file in `Client.React` project
* Find the following code:

```tsx
{path: '/signup', element: <SignUp/>},
{path: '/signup-external', element: <SignUpExternal/>},
```
and replace it with:

```tsx
{path: '/signup', element: <SignUpExtended/>},
{path: '/signup-external', element: <SignUpExternalExtended/>},
```

There should be already a commented out example of custom sign up flow in `/src/routes/index.tsx` file.


