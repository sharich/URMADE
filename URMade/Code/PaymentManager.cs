using System;
using Square.Connect.Api;
using Square.Connect.Model;

public static class PaymentManager
{
	private static string accessToken = "sandbox-sq0atb-KPMUjf8mGI_Khvy-ZUrQ3g";//ConfigurationManager.AppSettings["SquareUpAccessToken"];

	private static LocationApi locationApi			= new LocationApi();
	private static TransactionApi transactionApi	= new TransactionApi();
	private static CustomerApi customerApi			= new CustomerApi();
	private static CustomerCardApi cardApi			= new CustomerCardApi();

	private static string GetLocation()
	{
		ListLocationsResponse locations	= locationApi.ListLocations(accessToken);
		return locations.Errors == null ? locations.Locations[0].Id : null;
	}

	private static Money CreateMoney(decimal amount, string currency)
	{
		Money.CurrencyEnum Currency;
		if (!Enum.TryParse(currency, out Currency))
			return null;

		long Amount = 0;

		switch (Currency)
		{
			case Money.CurrencyEnum.USD:
				Amount = (long) Math.Ceiling(amount * 100.0m);
				break;

			default:
				return null;
		}

		return new Money(Amount, Currency);
	}

	public static bool Authenticate(string customerId, string customerCardId, ref string msg)
	{
		string idempotencyKey	= Guid.NewGuid().ToString();
		string location			= GetLocation();
		Money money				= CreateMoney(1.0m, "USD");
		ChargeRequest request	= new ChargeRequest(DelayCapture:	true,
													CustomerId:		customerId,
													CustomerCardId:	customerCardId,
													IdempotencyKey: idempotencyKey,
													AmountMoney:	money);

		try
		{
			ChargeResponse response = transactionApi.Charge(accessToken, location, request);

			if (response.Errors != null)
			{
				msg = response.Errors[0].Detail;
				return false;
			}
			else
			{
				transactionApi.VoidTransaction(accessToken, location, response.Transaction.Id);
				return true;
			}
		}
		catch
		{
			return false;
		}
	}

	public static bool Charge(string cardNonce, decimal amount, string currency, ref string msg)
	{
		string idempotencyKey	= Guid.NewGuid().ToString();
		Money money				= CreateMoney(amount, currency);
		ChargeRequest request	= new ChargeRequest(AmountMoney: money, IdempotencyKey: idempotencyKey, CardNonce: cardNonce);

		try
		{
			ChargeResponse response = transactionApi.Charge(accessToken, GetLocation(), request);

			if (response.Errors != null)
			{
				msg = response.Errors[0].Detail;
				return false;
			}
			else
				return true;
		}
		catch
		{
			return false;
		}
	}

	public static bool ChargeCard(string customerId, string customerCardId, decimal amount, string currency, ref string msg)
	{
		string idempotencyKey	= Guid.NewGuid().ToString();
		Money money				= CreateMoney(amount, currency);

		var request = new ChargeRequest(CustomerId:		customerId,
									    CustomerCardId:	customerCardId,
									    AmountMoney:	money,
									    IdempotencyKey:	idempotencyKey);

		try
		{
			var response = transactionApi.Charge(accessToken, GetLocation(), request);

			if (response.Errors != null)
			{
				msg = response.Errors[0].Detail;
				return false;
			}
			else
				return true;
		}
		catch
		{
			return false;
		}
	}

	public static string CreateCustomer(string firstName, string lastName, string emailAddress, string phoneNumber)
	{
		var request = new CreateCustomerRequest(GivenName:		firstName,
											    FamilyName:		lastName,
											    EmailAddress:	emailAddress,
											    PhoneNumber:	phoneNumber);

		try
		{
			var response = customerApi.CreateCustomer(accessToken, request);
			return response.Errors == null ? response.Customer.Id : null;
		}
		catch
		{
			return null;
		}
	}

	public static string CreateCustomerCard(string cardNonce, Address billingAddress, string holderName, string customerId)
	{
		var request = new CreateCustomerCardRequest(cardNonce, BillingAddress: billingAddress, CardholderName: holderName);

		try
		{
			var response = cardApi.CreateCustomerCard(accessToken, customerId, request);
			return response.Errors == null ? response.Card.Id : null;
		}
		catch (Exception e)
		{
			return null;
		}
	}

	public static bool DeleteCustomer(string customerId)
	{
		try
		{
			var response = customerApi.DeleteCustomer(accessToken, customerId);
			return response.Errors == null;
		}
		catch
		{
			return false;
		}
	}

	public static bool DeleteCard(string customerId, string cardId)
	{
		try
		{
			var response = cardApi.DeleteCustomerCard(accessToken, customerId, cardId);
			return response.Errors == null;
		}
		catch
		{
			return false;
		}
	}

	public static Transaction RetrieveTransaction(string transactionId, string locationId)
	{
		try
		{
			var response = transactionApi.RetrieveTransaction(accessToken, locationId, transactionId);
			return response.Errors == null ? response.Transaction : null;
		}
		catch
		{
			return null;
		}
	}
}
