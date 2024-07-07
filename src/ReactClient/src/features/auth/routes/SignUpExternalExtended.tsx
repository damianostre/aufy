import {Link, useNavigate} from "react-router-dom";
import {SubmitHandler, useForm} from "react-hook-form";
import {z} from "zod";
import {zodResolver} from "@hookform/resolvers/zod";
import {useState} from "react";
import {extractApiErrors} from "../../../lib/axios.ts";
import {useAuth} from "../../../providers/AuthProvider.tsx";

export const SignUpExternalExtended = () => {
    const [apiErrors, setApiErrors] = useState<string[]>();
    const auth = useAuth();
    const navigate = useNavigate();
    const {register, handleSubmit, formState: {isSubmitting, errors}} = useForm<FormModel>({
        resolver: zodResolver(validationSchema),
    });

    const onSubmit: SubmitHandler<FormModel> = data => {
        return auth.signUpExternal({
            aboutMe: data.aboutMe,
            mySiteUrl: data.mySiteUrl,
        }).then(() => {
            navigate("/profile")
        }).catch((error) => {
            setApiErrors(extractApiErrors(error) ?? ["Error occured"]);
        });
    };

    return <>
        <div className="flex min-h-full flex-1 flex-col justify-center py-6 sm:px-6 lg:px-8">
            <div className="sm:mx-auto sm:w-full sm:max-w-md">
                <h2 className="mt-6 text-center text-2xl font-bold leading-9 tracking-tight text-gray-900">
                    Sign up
                </h2>
            </div>

            <div className="mt-10 sm:mx-auto sm:w-full sm:max-w-[480px]">
                <div className="bg-white px-6 py-8 shadow sm:rounded-lg sm:px-12">
                    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5" method="POST" noValidate={true}>

                        {apiErrors && <div className="rounded-md bg-red-50 p-4">
                            <div className="flex">
                                <div className="ml-3">
                                    <div className="mt-2 text-sm text-red-700">
                                        <ul role="list" className="list-disc space-y-1 pl-5">
                                            {apiErrors.map((error, index) => <li key={index}>{error}</li>)}
                                        </ul>
                                    </div>
                                </div>
                            </div>
                        </div>}

                        <div>
                            <label htmlFor="aboutMe" className="block text-sm font-medium leading-6 text-gray-900">
                                About me
                            </label>
                            <div className="mt-2">
                                <input
                                    {...register("aboutMe")}
                                    type="text"
                                    className="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-inset focus:ring-black sm:text-sm sm:leading-6"
                                />
                            </div>
                            <p className="mt-2 text-sm text-red-600" id="email-error">
                                {errors.aboutMe?.message}
                            </p>
                        </div>

                        <div>
                            <label htmlFor="mySiteUrl" className="block text-sm font-medium leading-6 text-gray-900">
                                My Site URL
                            </label>
                            <div className="mt-2">
                                <input
                                    {...register("mySiteUrl")}
                                    type="text"
                                    autoComplete="email"
                                    className="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-inset focus:ring-black sm:text-sm sm:leading-6"
                                />
                            </div>
                            <p className="mt-2 text-sm text-red-600" id="email-error">
                                {errors.mySiteUrl?.message}
                            </p>
                        </div>

                        <div>
                            <button
                                type="submit"
                                className="flex w-full justify-center rounded-md hover:primary-bg primary-bg px-3 py-1.5 text-sm font-semibold leading-6 text-white shadow-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                                disabled={isSubmitting}
                            >
                                {isSubmitting ? "Signing up..." : "Sign up"}
                            </button>
                        </div>

                        <div className="flex items-center justify-between">
                            <div className="text-sm leading-6">
                                Already have an account?
                                <Link to="/signin" className="ml-3 font-semibold primary-color hover:primary-color">
                                    Sign in
                                </Link>
                            </div>
                        </div>
                    </form>
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
};

const validationSchema = z.object({
    aboutMe: z
        .string()
        .max(100, {message: "About me must be at most 100 characters"}),
    mySiteUrl: z
        .string()
        .url({message: "Must be a valid URL"}),
});
type FormModel = z.infer<typeof validationSchema>;