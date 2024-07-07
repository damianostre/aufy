import {useNavigate, useSearchParams} from "react-router-dom";
import {SubmitHandler, useForm} from "react-hook-form";
import {z} from "zod";
import {zodResolver} from "@hookform/resolvers/zod";
import {useState} from "react";
import {extractApiErrors} from "../../../lib/axios.ts";
import {auth} from "../../../api/auth.ts";

export const ResetPasswordForm = () => {
    const [params] = useSearchParams();
    const code = params.get("code");
    const navigate = useNavigate();
    if (!code){
        throw new Error("Invalid code");
    }

    const [apiErrors, setApiErrors] = useState<string[]>();

    const {register, handleSubmit, formState: {errors, isSubmitting}} = useForm<FormModel>({
        resolver: zodResolver(validationSchema),
    });
    const onSubmit: SubmitHandler<FormModel> = data => {
        auth.resetPassword({email: data.email, password: data.password, code: code}).then(() => {
            navigate("/reset-password/confirmation");
        }).catch((error) => {
            setApiErrors(extractApiErrors(error) ?? ["Error occured"]);
        });
    };

    return <form onSubmit={handleSubmit(onSubmit)} className="space-y-5" method="POST" noValidate={true}>

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
                    {...register("password", {required: true})}
                    type="password"
                    required
                    className="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-inset focus:ring-black sm:text-sm sm:leading-6"
                />
            </div>
            <p className="mt-2 text-sm text-red-600" id="email-error">
                {errors.password?.message}
            </p>

        </div>

        <div>
            <label htmlFor="password" className="block text-sm font-medium leading-6 text-gray-900">
                Confirm Password
            </label>
            <div className="mt-2">
                <input
                    {...register("confirmPassword", {
                        required: true,
                        validate: (value, formValues) => value === formValues.password
                    })}
                    type="password"
                    required
                    className="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-inset focus:ring-black sm:text-sm sm:leading-6"
                />
            </div>
            <p className="mt-2 text-sm text-red-600" id="confirmPassword-error">
                {errors.confirmPassword?.message}
            </p>
        </div>

        <div>
            <button
                type="submit"
                className="flex w-full justify-center rounded-md hover:primary-bg primary-bg px-3 py-1.5 text-sm font-semibold leading-6 text-white shadow-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                disabled={isSubmitting}
            >
                {isSubmitting ? "Processing..." : "Reset Password"}
            </button>
        </div>
    </form>
};

const validationSchema = z.object({
    email: z
        .string().min(1, {message: "Email is required"})
        .email({message: "Must be a valid email",}),
    password: z
        .string()
        .min(6, {message: "Password must be at least 6 characters"}),
    confirmPassword: z
        .string()
        .min(1, {message: "Confirm Password is required"}),
})
    .refine((data) => data.password === data.confirmPassword, {
        path: ["confirmPassword"],
        message: "Password don't match",
    });
type FormModel = z.infer<typeof validationSchema>;
