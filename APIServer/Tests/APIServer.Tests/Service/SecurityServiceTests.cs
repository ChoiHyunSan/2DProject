using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using APIServer;
using APIServer.Models.Entity;
using APIServer.Models.Entity.Data;
using APIServer.Models.Redis;
using APIServer.Repository;
using APIServer.Service.Implements;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SqlKata.Execution;
using Xunit;


namespace APIServer.Tests.Service;

public class SecurityServiceTests
{
    /*
     * Target   : HashPassword/VerifyPassword
     * Scenario : 올바른 비밀번호 검증 성공
     * Given    : GenerateSalt, HashPassword → hash
     * When     : VerifyPassword(hash, salt, input)
     * Then     : true
     */
    [Fact(DisplayName = "[Security] 비밀번호 해시/검증 성공")]
    [Trait("Target", "HashPassword/VerifyPassword")]
    public void HashAndVerify_Case01()
    {
        // Given
        var svc = new SecurityService();
        var salt = svc.GenerateSalt();
        var (ok, hash) = svc.HashPassword("pw123!", salt);

        // When
        var verified = svc.VerifyPassword(hash, salt, "pw123!");

        // Then
        ok.Should().BeTrue();
        verified.Should().BeTrue();
    }

    /*
     * Target   : GenerateSalt
     * Scenario : 솔트가 랜덤하게 생성됨
     * Given    : GenerateSalt 2회
     * When     : 비교
     * Then     : 서로 다름
     */
    [Fact(DisplayName = "[Security] 솔트 랜덤성")]
    [Trait("Target", "GenerateSalt")]
    public void GenerateSalt_Case01()
    {
        // Given
        var svc = new SecurityService();

        // When
        var s1 = svc.GenerateSalt();
        var s2 = svc.GenerateSalt();

        // Then
        s1.Should().NotBeNullOrEmpty();
        s2.Should().NotBeNullOrEmpty();
        s1.Should().NotBe(s2);
    }

    /*
     * Target   : GenerateAuthToken
     * Scenario : 토큰 생성
     * Given    : GenerateAuthToken
     * When     : 호출
     * Then     : 비어있지 않음
     */
    [Fact(DisplayName = "[Security] 인증 토큰 생성")]
    [Trait("Target", "GenerateAuthToken")]
    public void GenerateAuthToken_Case01()
    {
        // Given
        var svc = new SecurityService();

        // When
        var token = svc.GenerateAuthToken();

        // Then
        token.Should().NotBeNullOrEmpty();
    }
}
