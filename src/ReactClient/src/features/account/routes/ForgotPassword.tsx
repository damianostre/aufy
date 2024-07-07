import { z } from "zod";
import {zodResolver} from "@hookform/resolvers/zod";
import {SubmitHandler, useForm} from "react-hook-form";
import { auth } from "../../../api/auth";

export const ForgotPassword = () => {
    const {register, handleSubmit, formState: {errors}} = useForm<FormModel>({
        resolver: zodResolver(validationSchema),
    });
    const onSubmit: SubmitHandler<FormModel> = data => {
        auth.forgotPassword(data.email)
            .then(() => {
                alert("Password reset email sent");
            });
    };

    return (
        <>
            <div className="flex min-h-full flex-1 flex-col justify-center py-12 sm:px-6 lg:px-8">
                <div className="sm:mx-auto sm:w-full sm:max-w-md">
                    <h2 className="mt-6 text-center text-2xl font-bold leading-9 tracking-tight text-gray-900">
                        Forgot password?
                    </h2>
                </div>

                <div className="mt-10 sm:mx-auto sm:w-full sm:max-w-[480px]">
                    <div className="bg-white px-6 py-12 shadow sm:rounded-lg sm:px-12">
                        <form className="space-y-6" action="#" method="POST" onSubmit={handleSubmit(onSubmit)}
                              noValidate={true}>
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
                                        className="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-2 focus:ring-inset focus:ring-indigo-600 sm:text-sm sm:leading-6"
                                    />
                                </div>
                                <p className="mt-2 text-sm text-red-600" id="email-error">
                                    {errors.email?.message}
                                </p>
                            </div>

                            <div>
                                <button
                                    type="submit"
                                    className="flex w-full justify-center rounded-md primary-bg px-3 py-1.5 text-sm font-semibold leading-6 text-white shadow-sm hover:primary-bg focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:primary-bg-darker"
                                >
                                    Send password reset email
                                </button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </>
    );
};

const validationSchema = z.object({
    email: z
        .string().min(1, {message: "Email is required"})
        .email({message: "Must be a valid email",})
});
type FormModel = z.infer<typeof validationSchema>;
