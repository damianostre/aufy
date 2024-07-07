import {Link, useNavigate} from "react-router-dom";
import {SubmitHandler, useForm} from "react-hook-form";
import {z} from "zod";
import {zodResolver} from "@hookform/resolvers/zod";
import {useState} from "react";
import {extractApiErrors} from "../../../lib/axios.ts";
import {useAuth} from "../../../providers/AuthProvider.tsx";

export const SignInForm = () => {
    const [apiErrors, setApiErrors] = useState<string[]>();
    const navigate = useNavigate();
    const {register, handleSubmit, formState: {errors, isSubmitting}} = useForm<FormModel>({
        resolver: zodResolver(validationSchema),
    });

    const {signIn} = useAuth();
    const onSubmit: SubmitHandler<FormModel> = data => {
        signIn({email: data.email, password: data.password, rememberMe: true}).then(() => {
            navigate("/");
        }).catch((error) => {
            setApiErrors(extractApiErrors(error) ?? ["Error occured"]);
        });
    };

    return <form className="space-y-6" action="#" method="POST" onSubmit={handleSubmit(onSubmit)} noValidate={true}>

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
            <label htmlFor="email" className="block text-sm font-medium leading-6 text-gray-900">
                Email address
            </label>
            <div className="mt-2">
                <input
                    {...register("email")}
                    type="email"
                    autoComplete="email"
                    required
                    className="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-inset focus:ring-black sm:text-sm sm:leading-6"
                />
            </div>
            <p className="mt-2 text-sm text-red-600" id="email-error">
                {errors.email?.message}
            </p>
        </div>

        <div>
            <label htmlFor="password" className="block text-sm font-medium leading-6 text-gray-900">
                Password
            </label>
            <div className="mt-2">
                <input
                    {...register("password")}
                    type="password"
                    autoComplete="current-password"
                    required
                    className="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-inset focus:ring-black sm:text-sm sm:leading-6"
                />
            </div>
            <p className="mt-2 text-sm text-red-600" id="email-error">
                {errors.password?.message}
            </p>
        </div>

        <div className="flex items-center justify-between">
            <div className="flex items-center"></div>

            <div className="text-sm leading-6">
                <Link to="/forgot-password" className="font-semibold primary-color hover:primary-color">
                    Forgot password?
                </Link>
            </div>
        </div>

        <div>
            <button
                type="submit"
                className="flex w-full justify-center rounded-md primary-bg px-3 py-1.5 text-sm font-semibold leading-6 text-white shadow-sm hover:primary-bg focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:primary-bg-darker"
                disabled={isSubmitting}
            >
                {isSubmitting ? "Signing in..." : "Sign in"}
            </button>
        </div>

        <div className="flex items-center justify-between">
            <div className="text-sm leading-6">
                Not a member?
                <Link to="/signup" className="ml-3 font-semibold primary-color hover:primary-color">
                    Sign up now
                </Link>
            </div>
        </div>
    </form>
};

const validationSchema = z.object({
    email: z
        .string().min(1, {message: "Email is required"})
        .email({message: "Must be a valid email",}),
    password: z
        .string()
        .min(1, {message: "Password is required"}),
});
type FormModel = z.infer<typeof validationSchema>;
