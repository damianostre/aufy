import {SignInForm} from "../components/SignInForm.tsx";
import {ExternalProviders} from "../components/ExternalProviders.tsx";
import {Link} from "react-router-dom";

export const SignIn = () => {
    return (
        <>
            <div className="flex min-h-full flex-1 flex-col justify-center py-12 sm:px-6 lg:px-8">
                <div className="sm:mx-auto sm:w-full sm:max-w-md">
                    <h2 className="mt-6 text-center text-2xl font-bold leading-9 tracking-tight text-gray-900">
                        Sign in to your account
                    </h2>
                </div>

                <div className="mt-10 sm:mx-auto sm:w-full sm:max-w-[480px]">
                    <div className="bg-white px-6 py-12 shadow sm:rounded-lg sm:px-12">
                        <SignInForm/>

                        <div className="relative mt-10 mb-6">
                            <div className="absolute inset-0 flex items-center" aria-hidden="true">
                                <div className="w-full border-t border-gray-200"/>
                            </div>
                            <div className="relative flex justify-center text-sm font-medium leading-6">
                                <span className="bg-white px-6 text-gray-900">Or continue with</span>
                            </div>
                        </div>

                        <ExternalProviders mode="SignIn"/>
                    </div>
                    <div>
                        <Link to="/"
                              className="text-sm font-medium leading-6 text-gray-600 hover:text-gray-900">
                            <svg
                                className="w-4 h-4 inline-block"
                                viewBox="0 0 1024 1024"
                                xmlns="http://www.w3.org/2000/svg">
                            <path fill="#000000" d="M224 480h640a32 32 0 1 1 0 64H224a32 32 0 0 1 0-64z"/>
                                <path fill="#000000"
                                      d="m237.248 512 265.408 265.344a32 32 0 0 1-45.312 45.312l-288-288a32 32 0 0 1 0-45.312l288-288a32 32 0 1 1 45.312 45.312L237.248 512z"/>
                            </svg>
                            Home
                        </Link>
                    </div>
                </div>
            </div>
        </>
    );
};
